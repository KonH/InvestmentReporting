<template>
	<h1>Sell Asset</h1>
	<div class="form-group">
		<label>
			Date:
			<input ref="date" type="datetime-local" class="form-control" />
		</label>
	</div>
	<div class="form-group">
		<label>
			Asset:
			<select ref="asset" class="form-control">
				<option v-for="asset in inventory" :key="asset.id" :value="asset.id">
					{{ asset.isin }} {{ asset.name }} x{{ asset.count }}
				</option>
			</select>
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
	<button :onclick="onclick" class="btn btn-primary">Sell</button>
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
	name: 'SellAsset',
})
export default class SellAsset extends Vue {
	@State('activeState') activeState!: StateDto;

	@Ref('date')
	dateInput!: HTMLInputElement;

	@Ref('payAccount')
	payAccountSelect!: HTMLSelectElement;

	@Ref('feeAccount')
	feeAccountSelect!: HTMLSelectElement;

	@Ref('asset')
	assetSelect!: HTMLSelectElement;

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

	get inventory() {
		const broker = this.activeState.brokers?.find((b) => b.id == this.brokerId);
		if (!broker) {
			return [];
		}
		return broker.inventory;
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
			Backend.state().asset.sellAssetCreate({
				date: new Date(this.dateInput.value).toISOString(),
				broker: this.brokerId,
				payAccount: this.payAccountSelect.value,
				feeAccount: this.feeAccountSelect.value,
				asset: this.assetSelect.value,
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
