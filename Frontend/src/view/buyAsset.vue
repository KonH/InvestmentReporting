<template>
	<h1>Buy Asset</h1>
	<div class="form-group">
		<label>
			Date:
			<input ref="date" type="datetime-local" class="form-control" />
		</label>
	</div>
	<div class="form-group">
		<label>
			Name:
			<input ref="name" type="text" class="form-control" />
		</label>
	</div>
	<div class="form-group">
		<label>
			Category:
			<input ref="category" type="text" class="form-control" />
		</label>
	</div>
	<div class="form-group">
		<label>
			ISIN:
			<input ref="isin" type="text" class="form-control" />
		</label>
	</div>
	<div class="form-group">
		<label>
			Pay account:
			<select ref="payAccount" class="form-control">
				<option v-for="account in accounts" :key="account.id" :value="account.id">
					{{ account.displayName }}
				</option>
			</select>
		</label>
		<label class="ml-2">
			Price:
			<input ref="price" type="number" class="form-control" />
		</label>
	</div>
	<div class="form-group">
		<label>
			Fee account:
			<select ref="feeAccount" class="form-control">
				<option v-for="account in accounts" :key="account.id" :value="account.id">
					{{ account.displayName }}
				</option>
			</select>
		</label>
		<label class="ml-2">
			Fee:
			<input ref="fee" type="number" class="form-control" />
		</label>
	</div>
	<div class="form-group">
		<label>
			Count:
			<input ref="count" type="number" class="form-control" />
		</label>
	</div>
	<button :onclick="onclick" class="btn btn-primary">Buy</button>
	<router-link to="/" class="btn btn-secondary ml-2">Back</router-link>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import Backend from '@/service/backend';
import router from '@/router';
import { Action, State } from 'vuex-class';
import { StateDto } from '@/api/state';
import { Ref } from 'vue-property-decorator';

@Options({
	name: 'BuyAsset',
})
export default class BuyAsset extends Vue {
	@State('activeState') activeState!: StateDto;

	@Ref('date')
	dateInput!: HTMLInputElement;

	@Ref('payAccount')
	payAccountSelect!: HTMLSelectElement;

	@Ref('feeAccount')
	feeAccountSelect!: HTMLSelectElement;

	@Ref('name')
	nameInput!: HTMLInputElement;

	@Ref('category')
	categoryInput!: HTMLInputElement;

	@Ref('isin')
	isinInput!: HTMLInputElement;

	@Ref('price')
	priceInput!: HTMLInputElement;

	@Ref('fee')
	feeInput!: HTMLInputElement;

	@Ref('count')
	countInput!: HTMLInputElement;

	@Action('fetchState')
	fetchState!: () => void;

	get brokerId() {
		return this.$route.params.broker as string;
	}

	get accounts() {
		const broker = this.activeState.brokers?.find((b) => b.id == this.brokerId);
		if (!broker) {
			return [];
		}
		return broker.accounts;
	}

	mounted() {
		this.setCurrentDate();
	}

	setCurrentDate() {
		this.dateInput.value = new Date().toISOString();
	}

	async onclick() {
		const result = await Backend.tryFetch(
			Backend.state().asset.buyAssetCreate({
				date: new Date(this.dateInput.value).toISOString(),
				broker: this.brokerId,
				payAccount: this.payAccountSelect.value,
				feeAccount: this.feeAccountSelect.value,
				name: this.nameInput.value,
				category: this.categoryInput.value,
				isin: this.isinInput.value,
				price: Number.parseFloat(this.priceInput.value),
				fee: Number.parseFloat(this.feeInput.value),
				count: Number.parseFloat(this.countInput.value),
			})
		);
		if (result?.ok) {
			this.fetchState();
			await router.push('/');
		} else {
			alert(`Failed: ${result?.error}`);
		}
	}
}
</script>
