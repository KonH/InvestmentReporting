<template>
	<h1>Add Income</h1>
	<div class="form-group">
		<label>
			Date:
			<input ref="date" type="datetime-local" class="form-control" />
		</label>
	</div>
	<div class="form-group">
		<label>
			Amount:
			<input ref="amount" type="number" class="form-control" value="0" />
		</label>
	</div>
	<div class="form-group">
		<label>
			Category:
			<input ref="category" type="text" class="form-control" />
		</label>
	</div>
	<button :onclick="onclick" class="btn btn-primary">Add</button>
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
	name: 'AddIncome',
})
export default class AddIncome extends Vue {
	@State('activeState') activeState!: StateDto;

	@Ref('date')
	dateInput!: HTMLInputElement;

	@Ref('amount')
	amountInput!: HTMLInputElement;

	@Ref('category')
	categoryInput!: HTMLInputElement;

	@Action('fetchActiveState')
	fetchActiveState!: () => void;

	get brokerId() {
		return this.$route.params.broker as string;
	}

	get accountId() {
		return this.$route.params.account as string;
	}

	mounted() {
		this.setCurrentDate();
	}

	setCurrentDate() {
		this.dateInput.value = new Date().toISOString();
	}

	async onclick() {
		const result = await Backend.tryFetch(
			Backend.state().income.incomeCreate({
				date: new Date(this.dateInput.value).toISOString(),
				broker: this.brokerId,
				account: this.accountId,
				amount: Number.parseFloat(this.amountInput.value),
				category: this.categoryInput.value,
			})
		);
		if (result?.ok) {
			this.fetchActiveState();
			await router.push('/');
		} else {
			alert(`Failed: ${result?.error}`);
		}
	}
}
</script>
