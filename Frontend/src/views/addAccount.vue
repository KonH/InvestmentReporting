<template>
	<h1>Add Account</h1>
	<div class="form-group">
		<label>
			Name:
			<input ref="displayName" type="text" class="form-control" />
		</label>
	</div>
	<div>
		<label>
			Currency:
			<select
				v-for="currency in currencies"
				ref="currency"
				:key="currency.id"
				class="form-control"
			>
				<option :value="currency.id">
					{{ currency.code }} ({{ currency.format }})
				</option>
			</select>
		</label>
	</div>
	<button :onclick="onclick" class="btn btn-primary">Add</button>
	<router-link to="/" class="btn btn-secondary ml-2">Back</router-link>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import Backend from '@/service/backend';
import router from '@/router';
import { State } from 'vuex-class';
import { StateDto } from '@/api/state';

@Options({
	name: 'AddAccount',
})
export default class AddAccount extends Vue {
	@State('activeState') activeState!: StateDto;

	displayNameInput: HTMLInputElement | undefined;
	currencySelect: HTMLSelectElement | undefined;

	get currencies() {
		return this.activeState.currencies;
	}

	mounted() {
		this.displayNameInput = this.$refs.displayName as HTMLInputElement;
		this.currencySelect = this.$refs.currency as HTMLSelectElement;
	}

	async onclick() {
		if (!this.displayNameInput || !this.currencySelect) {
			console.error(
				`invalid setup (displayNameInput: ${this.displayNameInput}, currencySelect: ${this.currencySelect})`
			);
			return;
		}
		const brokerId = this.$route.params.broker as string;
		const currencyId = this.currencySelect.value;
		const result = await Backend.tryFetch(
			Backend.state().account.accountCreate({
				broker: brokerId,
				currency: currencyId,
				displayName: this.displayNameInput.value,
			})
		);
		if (result?.ok) {
			await router.push('/');
		} else {
			alert(`Failed: ${result?.error}`);
		}
	}
}
</script>
